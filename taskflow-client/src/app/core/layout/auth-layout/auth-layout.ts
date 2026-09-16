import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'tf-auth-layout',
  imports: [RouterLink, MatIcon, NgOptimizedImage],
  templateUrl: './auth-layout.html',
})
export class AuthLayout {
  readonly title = input.required<string>();
  readonly subtitle = input('');
}