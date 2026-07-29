import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { InstrumentHealthResponse, CreateInstrumentRequest } from '../models/run.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class InstrumentService {
  private readonly baseUrl = `${environment.apiBaseUrl}/instruments`;

  constructor(private readonly http: HttpClient) {}

  create(request: CreateInstrumentRequest): Observable<InstrumentHealthResponse> {
    return this.http.post<InstrumentHealthResponse>(this.baseUrl, request);
  }

  getHealth(id: string): Observable<InstrumentHealthResponse> {
    return this.http.get<InstrumentHealthResponse>(`${this.baseUrl}/${id}/health`);
  }
}
