import { Component } from "@angular/core";
import { AuthService } from "../../services/auth.service";
import { Router } from "@angular/router";
import { FormsModule } from "@angular/forms";

@Component({
  selector: "app-login",
  imports: [FormsModule],
  templateUrl: "./login.html",
  styleUrl: "./login.css",
})
export class Login {
  email = "";
  password = "";
  error = "";
  loading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  login(): void {
    this.loading = true;
    this.error = "";
    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.router.navigate(["/product"]);
      },
      error: (err) => {
        this.error = err.error?.message || "Login failed";
        this.loading = false;
      },
    });
  }
}
