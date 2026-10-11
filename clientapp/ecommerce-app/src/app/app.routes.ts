import { Routes } from "@angular/router";
import { Products } from "./components/products/products";
import { About } from "./components/about/about";
import { Contact } from "./components/contact/contact";
import { ProductDetails } from "./components/product-details/product-details";
import { Cart } from "./components/cart/cart";
import { Login } from "./components/login/login";
import { Register } from "./components/register/register";
import { AuthGuard } from "./guards/auth-guard";

export const routes: Routes = [
  { path: "", redirectTo: "products", pathMatch: "full" },
  { path: "products", component: Products, title: "Products" },
  { path: "about", component: About, title: "About" },
  { path: "contact", component: Contact, title: "Contact" },
  { path: "cart", component: Cart, title: "Cart" },
  { path: "product-details", component: ProductDetails, title: "Product Details" },
  { path: "login", component: Login, title: "Login"},
  { path: "register", component: Register, title: "Register"}
];
export default routes;
