import { createComponent, Injectable, inject, EnvironmentInjector, ApplicationRef } from '@angular/core';
import { ConfirmDialog, ConfirmOptions } from '../shared/confirm-dialog';

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  private readonly appRef = inject(ApplicationRef);
  private readonly envInjector = inject(EnvironmentInjector);

  confirm(options: ConfirmOptions): Promise<boolean> {
    return new Promise<boolean>(resolve => {
      const componentRef = createComponent(ConfirmDialog, {
        environmentInjector: this.envInjector,
      });
      componentRef.setInput('title', options.title);
      componentRef.setInput('message', options.message);
      componentRef.setInput('confirmText', options.confirmText ?? (options.danger ? 'Delete' : 'Confirm'));
      componentRef.setInput('cancelText', options.cancelText ?? 'Cancel');
      componentRef.setInput('danger', options.danger ?? false);

      const finish = (result: boolean): void => {
        subscription.unsubscribe();
        this.appRef.detachView(componentRef.hostView);
        componentRef.destroy();
        const el = componentRef.location.nativeElement as HTMLElement;
        el.remove();
        resolve(result);
      };

      const subscription = componentRef.instance.result.subscribe(finish);

      this.appRef.attachView(componentRef.hostView);
      document.body.appendChild(componentRef.location.nativeElement as HTMLElement);
      (componentRef.location.nativeElement as HTMLElement).focus();
    });
  }
}