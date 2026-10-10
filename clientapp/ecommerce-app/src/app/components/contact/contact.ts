import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-contact',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './contact.html',
  styleUrl: './contact.css',
})
export class Contact {}
