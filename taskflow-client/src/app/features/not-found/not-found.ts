import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink, MatIcon],
  template: `
    <div class="not-found">
      <div class="tf-empty">
        <mat-icon aria-hidden="true">explore_off</mat-icon>
        <h3>404 — Page not found</h3>
        <p>The page you're looking for doesn't exist or has moved.</p>
        <a class="tf-btn tf-btn-primary" routerLink="/projects">Back to projects</a>
      </div>
    </div>
  `,
})
export class NotFound {}