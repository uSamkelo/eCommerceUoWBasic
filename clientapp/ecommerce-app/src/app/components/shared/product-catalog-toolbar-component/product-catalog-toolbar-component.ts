import { Component, input, output } from "@angular/core";
import { IProduct } from "../../../Interfaces/IProduct";

@Component({
  selector: "app-product-catalog-toolbar-component",
  imports: [],
  templateUrl: "./product-catalog-toolbar-component.html",
  styleUrl: "./product-catalog-toolbar-component.css",
})
export class ProductCatalogToolbarComponent {
  readonly totalItems = input(0);
  readonly searchText = input("");
  readonly suggestions = input<IProduct[]>([]);
  readonly categories = input<string[]>([]);
  readonly selectedCategory = input("");
  readonly searchChange = output<string>();
  readonly sortChange = output<string>();
  readonly categoryChange = output<string>();

  onSearchSubmit(event: SubmitEvent) {
    event.preventDefault();

    const form = event.currentTarget;
    if (form instanceof HTMLFormElement) {
      const formData = new FormData(form);
      this.searchChange.emit(String(formData.get("search") ?? ""));
    }
  }

  onSearchInput(event: Event) {
    const input = event.target;

    if (input instanceof HTMLInputElement) {
      this.searchChange.emit(input.value);
    }
  }

  onSortChange(event: Event) {
    const select = event.target;

    if (select instanceof HTMLSelectElement) {
      this.sortChange.emit(select.value);
    }
  }

  onCategoryChange(event: Event) {
    const select = event.target;

    if (select instanceof HTMLSelectElement) {
      this.categoryChange.emit(select.value);
    }
  }
}
