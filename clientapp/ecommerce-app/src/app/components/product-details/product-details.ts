import { Component } from "@angular/core";
import { CartService } from "../../services/cart";
import { ActivatedRoute } from "@angular/router";
import { IProduct } from "../../Interfaces/IProduct";
import { CurrencyPipe } from "@angular/common";
import { ProductService } from "../../services/product";
import { FontAwesomeModule } from "@fortawesome/angular-fontawesome";

@Component({
  selector: "app-product-details",
  imports: [CurrencyPipe, FontAwesomeModule],
  templateUrl: "./product-details.html",
  styleUrl: "./product-details.css",
})
export class ProductDetails {
  product: IProduct | undefined;
  constructor(
    private route: ActivatedRoute,
    private cartService: CartService,
    private productService: ProductService,
  ) {
  }

  addToCart(product: IProduct) {
    this.cartService.addToCart(product);
    window.alert("Your product has been added to the cart!");
  }

  ngOnInit() {
    // First get the product id from the current route.
    const productId = Number(this.route.snapshot.queryParamMap.get("productId"));
    if (!Number.isInteger(productId) || productId < 1) {
      return;
    }

    this.productService.getProduct(productId).subscribe((product) => {
      this.product = product;
    });
  }
}
