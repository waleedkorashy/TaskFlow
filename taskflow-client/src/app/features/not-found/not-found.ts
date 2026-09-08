import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="not-found">
      <h1>404</h1>
      <p>This page doesn't exist.</p>
      <a routerLink="/projects">Go back to your projects</a>
    </div>
  `,
  styles: [`
    .not-found {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      background: #f4f5f7;
      text-align: center;
      h1 { font-size: 3rem; margin: 0; color: #3f51b5; }
      a { color: #3f51b5; margin-top: 1rem; }
    }
  `]
})
export class NotFound {}