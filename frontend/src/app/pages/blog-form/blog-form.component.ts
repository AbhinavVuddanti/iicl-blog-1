import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { BlogService } from '../../core/blog/blog.service';
import { BlogPost } from '../../core/blog/blog.model';

@Component({
  selector: 'app-blog-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatSnackBarModule, MatCardModule],
  templateUrl: './blog-form.component.html',
  styleUrl: './blog-form.component.scss'
})
export class BlogFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private api = inject(BlogService);
  private snack = inject(MatSnackBar);

  form!: FormGroup;
  loading = false;
  error: string | null = null;
  validationErrors: string[] = [];
  isEdit = false;
  id: number | null = null;

  ngOnInit(): void {
    this.form = this.fb.group({
      id: [0],
      title: ['', [Validators.required, Validators.maxLength(150)]],
      content: ['', [Validators.required, Validators.minLength(10)]],
      author: ['', [Validators.required, Validators.maxLength(100)]],
    });

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEdit = true;
      this.id = Number(idParam);
      this.load(this.id);
    }
  }

  load(id: number) {
    this.loading = true;
    this.api.getById(id).subscribe({
      next: (p: BlogPost) => {
        this.form.patchValue(p);
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load post';
        this.loading = false;
      }
    });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    const value = this.form.value as BlogPost;
    const req = this.isEdit
      ? this.api.update(value)
      : this.api.create({ title: value.title, content: value.content, author: value.author });
    req.subscribe({
      next: (p) => {
        this.loading = false;
        this.snack.open(this.isEdit ? 'Post updated' : 'Post created', 'OK', { duration: 2000 });
        this.router.navigate(['/blogs', (p as BlogPost).id]);
      },
      error: (err) => {
        this.error = 'Failed to save';
        this.validationErrors = [];
        if (err?.error) {
          if (typeof err.error === 'string') {
            this.validationErrors.push(err.error);
          } else if (err.error.errors) {
            // ProblemDetails format
            for (const k of Object.keys(err.error.errors)) {
              const arr = err.error.errors[k];
              if (Array.isArray(arr)) arr.forEach((m: string) => this.validationErrors.push(m));
            }
          } else if (err.error.title) {
            this.validationErrors.push(err.error.title);
          }
        }
        this.snack.open('Failed to save', 'Dismiss', { duration: 3000 });
        this.loading = false;
      }
    });
  }
}
