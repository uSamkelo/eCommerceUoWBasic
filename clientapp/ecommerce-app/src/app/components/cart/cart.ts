import { CurrencyPipe } from '@angular/common';
import { Component } from '@angular/core';
import { CartItem, CartService } from '../../services/cart';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faMinus, faPlus, faTrash } from '@fortawesome/free-solid-svg-icons';
import { IPurchaseRequest } from '../../Interfaces/IPurchase';
import { PurchaseService } from '../../services/purchase-service';

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
  
  isCheckingOut = false;
  checkoutError: string | null = null;
  checkoutMessage: string | null = null;
  private pendingCheckout:
  | { signature: string; key: string }
  | null = null;

  constructor(private cartService: CartService, private purchaseService: PurchaseService) {
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

  checkout() {
  if (this.isCheckingOut || this.cartItems.length === 0) {
    return;
  }

  const items: IPurchaseRequest[] = this.cartItems
    .map(({ product, quantity }) => ({
      productId: product.id,
      quantity,
    }))
    .sort((a, b) => a.productId - b.productId);

  const signature = JSON.stringify(items);

  if (this.pendingCheckout?.signature !== signature) {
    this.pendingCheckout = {
      signature,
      key: crypto.randomUUID(),
    };
  }

  this.isCheckingOut = true;
  this.checkoutError = null;
  this.checkoutMessage = null;

  this.purchaseService
    .purchaseProduct(items, this.pendingCheckout.key)
    .subscribe({
      next: (response) => {
        this.checkoutMessage = response.message;
        this.pendingCheckout = null;
        this.cartItems = this.cartService.clearCart();
      },
      error: (error: { error?: { message?: string } }) => {
        this.checkoutError =
          error.error?.message ?? 'Purchase failed. Please try again.';
      },
      complete: () => {
        this.isCheckingOut = false;
      },
    });
}

  clearCart() {
    this.cartItems = this.cartService.clearCart();
  }
}
