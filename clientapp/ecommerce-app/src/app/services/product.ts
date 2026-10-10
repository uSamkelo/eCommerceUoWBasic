import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IProduct } from '../Interfaces/IProduct';

export interface PagedResponse<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class ProductService
{
  baseUrl = 'https://localhost:7037/api/Product';
  constructor(private http: HttpClient) {
 
  }

  getProducts(pageNumber = 1, pageSize = 20, sortBy = 'Name', search = ''): Observable<PagedResponse<IProduct>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize)
      .set('sortBy', sortBy);

    if (search.trim()) {
      params = params.set('search', search.trim());
    }  
    return this.http.get<PagedResponse<IProduct>>(this.baseUrl, { params });
  }

  getProduct(id: number): Observable<IProduct> {
    return this.http.get<IProduct>(`${this.baseUrl}/${id}`);
  }
}
