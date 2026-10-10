import { CurrencyPipe } from '@angular/common';
import { Component } from '@angular/core';
import { CartItem, CartService } from '../../services/cart';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faMinus, faPlus, faTrash } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-cart',
  imports: [CurrencyPipe, RouterLink, FontAwesomeModule],
  templateUrl: './cart.html',
  styleUrl: './cart.css',
})
export class Cart {
  cartItems: CartItem[];
  readonly faTrash = faTrash;
  readonly faMinus = faMinus;
  readonly faPlus = faPlus;

  constructor(private cartService: CartService) {
    this.cartItems = cartService.getItems();
  }

  get totalQuantity() {
    return this.cartService.getTotalQuantity();
  }

  get totalPrice() {
    return this.cartService.getTotalPrice();
  }

  decreaseQuantity(productId: number) {
    this.cartService.decreaseQuantity(productId);
    this.cartItems = this.cartService.getItems();
  }

  increaseQuantity(productId: number) {
    this.cartService.increaseQuantity(productId);
    this.cartItems = this.cartService.getItems();
  }

  removeItem(productId: number) {
    this.cartService.removeItem(productId);
    this.cartItems = this.cartService.getItems();
  }

  clearCart() {
    this.cartItems = this.cartService.clearCart();
  }
}
