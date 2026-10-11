import { CurrencyPipe } from '@angular/common';
import { Component } from '@angular/core';
import { CartItem, CartService } from '../../services/cart';
import { Router, RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faMinus, faPlus, faTrash } from '@fortawesome/free-solid-svg-icons';
import { IPurchaseRequest } from '../../Interfaces/IPurchase';
import { PurchaseService } from '../../services/purchase-service';
import { ToastService } from '../../services/toast.service';
import { finalize } from 'rxjs';

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
  checkoutMessage: string | null = null;
  private pendingCheckout:
  | { signature: string; key: string }
  | null = null;

  constructor(
    private cartService: CartService,
    private purchaseService: PurchaseService,
    private toastService: ToastService,
    private router: Router,
  ) {
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
  this.checkoutMessage = null;

  this.purchaseService
    .purchaseProduct(items, this.pendingCheckout.key)
    .pipe(finalize(() => {
      this.isCheckingOut = false;
    }))
    .subscribe({
      next: (response) => {
        this.checkoutMessage = response.message;
        this.pendingCheckout = null;
        this.cartItems = this.cartService.clearCart();
        void this.router.navigate(['/products']);
      },
      error: (error: { status?: number; error?: unknown }) => {
        if (error.status === 409) {
          this.pendingCheckout = null;
        }

        const body =
          typeof error.error === 'object' && error.error !== null
            ? (error.error as Record<string, unknown>)
            : {};
        const serverMessage =
          typeof body['message'] === 'string'
            ? body['message']
            : 'Purchase failed. Please try again.';
        const productId =
          typeof body['productId'] === 'number' ? body['productId'] : null;
        const failedProduct = error.status === 409 && productId !== null
          ? this.cartItems.find(
              (item) => item.product.id === productId,
            )
          : undefined;

        const message = failedProduct
          ? `${failedProduct.product.name}: ${serverMessage}`
          : serverMessage;
        this.toastService.error(message, 6000);
      },
    });
}

  clearCart() {
    this.cartItems = this.cartService.clearCart();
  }
}
