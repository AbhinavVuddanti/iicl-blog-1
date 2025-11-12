import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BlogService } from '../../core/blog/blog.service';
import { BlogPost, PagedResult } from '../../core/blog/blog.model';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-blog-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, MatIconModule, FormsModule, MatDialogModule],
  templateUrl: './blog-list.component.html',
  styleUrl: './blog-list.component.scss'
})
export class BlogListComponent implements OnInit {
  private api = inject(BlogService);
  private dialog = inject(MatDialog);

  loading = false;
  error: string | null = null;
  page = 1;
  pageSize = 10;
  total = 0;
  items: BlogPost[] = [];

  author = '';
  search = '';

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.error = null;
    this.api.getAll({ page: this.page, pageSize: this.pageSize, author: this.author || undefined, search: this.search || undefined })
      .subscribe({
        next: (res: PagedResult<BlogPost>) => {
          this.items = res.items;
          this.total = res.total;
          this.loading = false;
        },
        error: (err) => {
          this.error = 'Failed to load posts';
          this.loading = false;
        }
      });
  }

  nextPage() {
    if (this.page * this.pageSize < this.total) {
      this.page++;
      this.load();
    }
  }

  prevPage() {
    if (this.page > 1) {
      this.page--;
      this.load();
    }
  }

  get totalPages() {
    return Math.max(1, Math.ceil(this.total / this.pageSize));
  }

  delete(item: BlogPost) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete Post', message: `Delete "${item.title}"?` }
    });
    ref.afterClosed().subscribe((ok) => {
      if (ok) {
        this.api.delete(item.id).subscribe({
          next: () => this.load()
        });
      }
    });
  }
}
