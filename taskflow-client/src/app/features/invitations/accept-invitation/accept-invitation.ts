import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { InvitationsService } from '../../../core/services/invitations.service';
import { AuthService } from '../../../core/services/auth.service';
import { InvitationPreview } from '../../../core/models/invitation.models';

@Component({
  selector: 'app-accept-invitation',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './accept-invitation.html',
  styleUrl: './accept-invitation.scss'
})
export class AcceptInvitation implements OnInit {
  protected preview = signal<InvitationPreview | null>(null);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);
  protected isAccepting = signal(false);
  protected accepted = signal(false);

  private token!: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private invitationsService: InvitationsService,
    protected authService: AuthService
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.paramMap.get('token')!;

    this.invitationsService.preview(this.token).subscribe({
      next: (preview) => {
        this.preview.set(preview);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('This invitation link is invalid.');
        this.isLoading.set(false);
      }
    });
  }

  protected onAccept(): void {
    this.isAccepting.set(true);
    this.invitationsService.accept(this.token).subscribe({
      next: () => {
        this.isAccepting.set(false);
        this.accepted.set(true);
        setTimeout(() => this.router.navigate(['/projects']), 1500);
      },
      error: (err) => {
        this.isAccepting.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Could not accept invitation.');
      }
    });
  }
}