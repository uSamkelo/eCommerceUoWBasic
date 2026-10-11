import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { IPurchaseRequest, IPurchaseResponse } from '../Interfaces/IPurchase';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PurchaseService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {
    
  }

  purchaseProduct(
    items: IPurchaseRequest[],
    idempotencyKey: string,
  ): Observable<IPurchaseResponse> {
    return this.http.post<IPurchaseResponse>(
      `${this.baseUrl}/purchases`,
      items,
      {
        headers: new HttpHeaders({
          'Idempotency-Key': idempotencyKey,
        }),
      },
    );
  }
}
