import { Injectable, signal, effect, DestroyRef, inject } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ConcentrationModeService {
  private _isConcentrationMode = signal<boolean>(false);
  private readonly destroyRef = inject(DestroyRef);
  private fullscreenEffect?: ReturnType<typeof effect>;
  
  isConcentrationMode = this._isConcentrationMode.asReadonly();

  constructor() {
    // Sincronizar con el estado de pantalla completa del navegador
    this.setupFullscreenListeners();
    
    // Sincronizar el signal con el estado de pantalla completa
    this.fullscreenEffect = effect(() => {
      const isActive = this._isConcentrationMode();
      if (isActive && !this.isFullscreen()) {
        this.enterFullscreen();
      } else if (!isActive && this.isFullscreen()) {
        this.exitFullscreen();
      }
    });

    // Limpiar el effect cuando el servicio se destruya
    this.destroyRef.onDestroy(() => {
      this.fullscreenEffect?.destroy();
    });
  }

  private setupFullscreenListeners(): void {
    // Escuchar cambios en el estado de pantalla completa
    document.addEventListener('fullscreenchange', () => {
      if (!this.isFullscreen() && this._isConcentrationMode()) {
        // Si salimos de pantalla completa manualmente, desactivar modo concentración
        this._isConcentrationMode.set(false);
      }
    });

    document.addEventListener('webkitfullscreenchange', () => {
      if (!this.isFullscreen() && this._isConcentrationMode()) {
        this._isConcentrationMode.set(false);
      }
    });

    document.addEventListener('mozfullscreenchange', () => {
      if (!this.isFullscreen() && this._isConcentrationMode()) {
        this._isConcentrationMode.set(false);
      }
    });

    document.addEventListener('MSFullscreenChange', () => {
      if (!this.isFullscreen() && this._isConcentrationMode()) {
        this._isConcentrationMode.set(false);
      }
    });
  }

  private isFullscreen(): boolean {
    return !!(
      document.fullscreenElement ||
      (document as any).webkitFullscreenElement ||
      (document as any).mozFullScreenElement ||
      (document as any).msFullscreenElement
    );
  }

  private async enterFullscreen(): Promise<void> {
    const element = document.documentElement;
    
    try {
      if (element.requestFullscreen) {
        await element.requestFullscreen();
      } else if ((element as any).webkitRequestFullscreen) {
        await (element as any).webkitRequestFullscreen();
      } else if ((element as any).mozRequestFullScreen) {
        await (element as any).mozRequestFullScreen();
      } else if ((element as any).msRequestFullscreen) {
        await (element as any).msRequestFullscreen();
      }
    } catch (error) {
      console.error('Error al entrar en pantalla completa:', error);
      // Si falla, al menos activar el modo concentración sin pantalla completa
      this._isConcentrationMode.set(true);
    }
  }

  private async exitFullscreen(): Promise<void> {
    try {
      if (document.exitFullscreen) {
        await document.exitFullscreen();
      } else if ((document as any).webkitExitFullscreen) {
        await (document as any).webkitExitFullscreen();
      } else if ((document as any).mozCancelFullScreen) {
        await (document as any).mozCancelFullScreen();
      } else if ((document as any).msExitFullscreen) {
        await (document as any).msExitFullscreen();
      }
    } catch (error) {
      console.error('Error al salir de pantalla completa:', error);
    }
  }

  toggle(): void {
    const newState = !this._isConcentrationMode();
    this._isConcentrationMode.set(newState);
  }

  enable(): void {
    this._isConcentrationMode.set(true);
  }

  disable(): void {
    this._isConcentrationMode.set(false);
  }
}

