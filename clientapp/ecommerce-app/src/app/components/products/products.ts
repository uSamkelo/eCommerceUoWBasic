import { Component, signal } from "@angular/core";
import { CurrencyPipe } from "@angular/common";
import { IProduct } from "../../Interfaces/IProduct";
import { PagedResponse, ProductService } from "../../services/product";
import { CartService } from "../../services/cart";
import { RouterLink } from "@angular/router";
import { FontAwesomeModule } from "@fortawesome/angular-fontawesome";
import { faCirclePlus } from "@fortawesome/free-solid-svg-icons";
import { ProductCatalogToolbarComponent } from "../shared/product-catalog-toolbar-component/product-catalog-toolbar-component";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { catchError, EMPTY, Subject, switchMap, tap, timer } from "rxjs";

@Component({
  selector: "app-products",
  imports: [
    CurrencyPipe,
    RouterLink,
    FontAwesomeModule,
    ProductCatalogToolbarComponent,
  ],
  templateUrl: "./products.html",
  styleUrl: "./products.css",
})
export class Products {
  readonly faCirclePlus = faCirclePlus;
  readonly productsPage = signal<PagedResponse<IProduct> | null>(null);
  readonly pageNumber = signal(1);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly categoryErrorMessage = signal<string | null>(null);
  readonly search = signal("");
  readonly sortBy = signal("Name");
  readonly category = signal("");
  readonly categories = signal<string[]>([]);

  constructor(
    private productService: ProductService,
    private cartService: CartService,
  ) {
    this.productRequests.pipe(
      switchMap((request) =>
        timer(250).pipe(
          switchMap(() =>
            this.productService.getProducts(
              request.pageNumber,
              20,
              request.sortBy,
              request.search,
              request.category,
            )
          ),
          tap((response) => {
            this.productsPage.set(response);
            this.loading.set(false);
          }),
          catchError((error: unknown) => {
            console.error("Failed to load products.", error);
            this.errorMessage.set("Could not load products. Please try again.");
            this.loading.set(false);
            return EMPTY;
          }),
        )
      ),
      takeUntilDestroyed(),
    ).subscribe();

    this.productService.getProductCategories().pipe(
      tap((categories) => this.categories.set(categories)),
      catchError((error: unknown) => {
        console.error("Failed to load product categories.", error);
        this.categoryErrorMessage.set("Could not load product categories.");
        return EMPTY;
      }),
      takeUntilDestroyed(),
    ).subscribe();

    this.loadProducts();
  }

  addToCart(product: IProduct) {
    this.cartService.addToCart(product);
  }

  loadProducts(pageNumber = this.pageNumber()) {
    this.pageNumber.set(pageNumber);
    this.loading.set(true);
    this.errorMessage.set(null);

    this.productRequests.next({
      pageNumber,
      sortBy: this.sortBy(),
      search: this.search(),
      category: this.category(),
    });
  }

  onSearchChange(search: string) {
    this.search.set(search);
    this.loadProducts(1);
  }

  onSortChange(sortBy: string) {
    this.sortBy.set(sortBy);
    this.loadProducts(1);
  }

  onCategoryChange(category: string) {
    this.category.set(category);
    this.loadProducts(1);
  }

  private readonly productRequests = new Subject<{
    pageNumber: number;
    sortBy: string;
    search: string;
    category: string;
  }>();
}
