import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BlogPost, PagedResult } from './blog.model';

@Injectable({
  providedIn: 'root'
})
export class BlogService {
  private http = inject(HttpClient);
  private base = environment.apiBase + '/blogs';

  create(post: Partial<BlogPost>): Observable<BlogPost> {
    const { id, createdAt, updatedAt, ...rest } = post as any;
    return this.http.post<BlogPost>(this.base, rest);
  }

  getAll(options?: { page?: number; pageSize?: number; author?: string; from?: string; to?: string; search?: string; }): Observable<PagedResult<BlogPost>> {
    let params = new HttpParams();
    if (options?.page) params = params.set('page', options.page);
    if (options?.pageSize) params = params.set('pageSize', options.pageSize);
    if (options?.author) params = params.set('author', options.author);
    if (options?.from) params = params.set('from', options.from);
    if (options?.to) params = params.set('to', options.to);
    if (options?.search) params = params.set('search', options.search);
    return this.http.get<PagedResult<BlogPost>>(this.base, { params });
  }

  getById(id: number): Observable<BlogPost> {
    return this.http.get<BlogPost>(`${this.base}/${id}`);
  }

  update(post: BlogPost): Observable<BlogPost> {
    return this.http.put<BlogPost>(`${this.base}/${post.id}`, post);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
