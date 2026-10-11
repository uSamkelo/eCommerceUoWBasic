export interface IPurchaseRequest {
  productId: number;
  quantity: number;
}

export interface IPurchaseResponse {
  message: string;
  productId?: number;
}