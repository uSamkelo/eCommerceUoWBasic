import { Component } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faCartShopping } from '@fortawesome/free-solid-svg-icons';
import { CartService } from '../../../services/cart';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-navbar',
  imports: [AsyncPipe, RouterLink, RouterLinkActive, FontAwesomeModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  routes: { path: string, label: string }[];
  authRoutes: { path: string; label: string }[] = [
  { path: '/register', label: 'Register' },
  { path: '/login', label: 'Login' }
];
  readonly faCartShopping = faCartShopping;
  private cartService: CartService;

  constructor(
    private router: Router,
    cartService: CartService,
    public authService: AuthService,
  ) {
    this.cartService = cartService;
    this.routes = this.router.config.flatMap(route => {
      const path = route.path;
      const label = route.title;

      const isAuthRoute = this.authRoutes.some(
        authRoute => authRoute.path.replace(/^\//, '') === path,
      );

      return path && !isAuthRoute && typeof label === 'string'
        ? [{ path, label }]
        : [];
    });
  }

  get cartCount(): number {
    return this.cartService.getTotalQuantity();
  }

  signOut(): void {
    this.authService.logout();
    void this.router.navigate(['/products']);
  }
}
