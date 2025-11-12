import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { BlogService } from '../../core/blog/blog.service';
import { BlogPost } from '../../core/blog/blog.model';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-blog-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, MatIconModule, MatDialogModule],
  templateUrl: './blog-detail.component.html',
  styleUrl: './blog-detail.component.scss'
})
export class BlogDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private api = inject(BlogService);
  private dialog = inject(MatDialog);

  loading = false;
  error: string | null = null;
  post: BlogPost | null = null;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) this.load(id);
  }

  load(id: number) {
    this.loading = true;
    this.api.getById(id).subscribe({
      next: (p) => { this.post = p; this.loading = false; },
      error: () => { this.error = 'Post not found'; this.loading = false; }
    });
  }

  delete() {
    if (!this.post) return;
    const ref = this.dialog.open(ConfirmDialogComponent, { data: { title: 'Delete Post', message: 'Are you sure you want to delete this post?' } });
    ref.afterClosed().subscribe(ok => {
      if (ok) {
        this.api.delete(this.post!.id).subscribe({
          next: () => this.router.navigate(['/blogs'])
        });
      }
    });
  }
}
