import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
} from "@angular/core";
import { provideRouter, withComponentInputBinding } from "@angular/router";

import { routes } from "./app.routes";
import {
  provideClientHydration,
  withEventReplay,
} from "@angular/platform-browser";
import { DEFAULT_CURRENCY_CODE, LOCALE_ID } from "@angular/core";
import { registerLocaleData } from '@angular/common';
import localeEnZa from '@angular/common/locales/en-ZA';
import { HTTP_INTERCEPTORS, provideHttpClient, withFetch, withInterceptorsFromDi } from "@angular/common/http";
import { JwtInterceptor } from "./interceptors/jwt-interceptor";

registerLocaleData(localeEnZa);

export const appConfig: ApplicationConfig = {
  providers: [
    { provide: LOCALE_ID, useValue: "en-ZA" },
    { provide: DEFAULT_CURRENCY_CODE, useValue: "ZAR" },
    provideHttpClient(withFetch()),
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideClientHydration(withEventReplay()),
    provideHttpClient(withFetch(), withInterceptorsFromDi()),
   { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true }
  ],
};
