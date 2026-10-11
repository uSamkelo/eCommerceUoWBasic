import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './components/shared/navbar/navbar';
import { Toast } from './components/shared/toast/toast';
import { registerLocaleData } from '@angular/common';
import localeEnZa from '@angular/common/locales/en-ZA';

registerLocaleData(localeEnZa);
@Component({
  selector: 'app-root',
  imports: [Navbar, RouterOutlet, Toast],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('ecommerce-app');

}
