import { trigger, transition, style, animate, query } from '@angular/animations';

export const routeAnimations = trigger('routeAnimations', [
  transition('* <=> *', [
    // Estilo inicial: componente saliente
    query(':leave', [
      style({ opacity: 1, transform: 'translateX(0)' })
    ], { optional: true }),
    
    // Estilo inicial: componente entrante
    query(':enter', [
      style({ opacity: 0, transform: 'translateX(20px)' })
    ], { optional: true }),
    
    // Animar salida
    query(':leave', [
      animate('200ms ease-out', style({ 
        opacity: 0, 
        transform: 'translateX(-20px)' 
      }))
    ], { optional: true }),
    
    // Animar entrada
    query(':enter', [
      animate('300ms ease-out', style({ 
        opacity: 1, 
        transform: 'translateX(0)' 
      }))
    ], { optional: true })
  ])
]);

export const fadeSlideAnimation = trigger('fadeSlide', [
  transition(':enter', [
    style({ 
      opacity: 0, 
      transform: 'translateY(10px)' 
    }),
    animate('300ms ease-out', style({ 
      opacity: 1, 
      transform: 'translateY(0)' 
    }))
  ]),
  transition(':leave', [
    animate('200ms ease-in', style({ 
      opacity: 0, 
      transform: 'translateY(-10px)' 
    }))
  ])
]);

