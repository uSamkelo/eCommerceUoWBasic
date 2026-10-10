import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faCartShopping } from '@fortawesome/free-solid-svg-icons';
import { CartService } from '../../../services/cart';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive, FontAwesomeModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  routes: { path: string, label: string }[];
  readonly faCartShopping = faCartShopping;
  private cartService: CartService;

  constructor(private router: Router, cartService: CartService) {
    this.cartService = cartService;
    this.routes = this.router.config.flatMap(route => {
      const path = route.path;
      const label = route.title;

      return path && path !== 'cart' && typeof label === 'string' ? [{ path, label }] : [];
    });
  }

  get cartCount(): number {
    return this.cartService.getTotalQuantity();
  }
}
