import { Injectable } from '@angular/core';
import { IProduct } from '../Interfaces/IProduct';

export interface CartItem {
  product: IProduct;
  quantity: number;
}

@Injectable({
  providedIn: 'root',
})
export class CartService {
  items: CartItem[] = [];

  addToCart(product: IProduct) {
    const existingItem = this.items.find((item) => item.product.id === product.id);

    if (existingItem) {
      existingItem.quantity += 1;
      return;
    }

    this.items.push({ product, quantity: 1 });
  }

  getItems() {
    return this.items;
  }

  decreaseQuantity(productId: number) {
    const item = this.items.find((cartItem) => cartItem.product.id === productId);

    if (!item) {
      return;
    }

    if (item.quantity > 1) {
      item.quantity -= 1;
      return;
    }

    this.removeItem(productId);
  }

  increaseQuantity(productId: number) {
    const item = this.items.find((cartItem) => cartItem.product.id === productId);

    if (!item) {
      return;
    }
    
    item.quantity += 1;
  }

  removeItem(productId: number) {
    this.items = this.items.filter((item) => item.product.id !== productId);
  }

  getTotalQuantity() {
    return this.items.reduce((total, item) => total + item.quantity, 0);
  }

  getTotalPrice() {
    return this.items.reduce(
      (total, item) => total + item.product.price * item.quantity,
      0,
    );
  }

  clearCart() {
    this.items = [];
    return this.items;
  }
}
