import { Component } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router } from "@angular/router";
import { AuthService } from "../../services/auth.service";

@Component({
  selector: "app-register",
  imports: [FormsModule],
  templateUrl: "./register.html",
  styleUrl: "./register.css",
})
export class Register {
  username = "";
  email = "";
  password = "";
  error = "";
  loading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  register() {
    if (this.loading) {
      return;
    }

    this.loading = true;
    this.error = "";
    this.authService.register(this.email, this.password, this.username).subscribe({
      next: () => {
        this.router.navigate(["/products"]);
      },
      error: (err) => {
        this.error =
          err.error?.message ??
          err.error?.errors?.map((item: { description: string }) => item.description).join(" ") ??
          "Registration failed.";
        this.loading = false;
      },
    });
  }
}
