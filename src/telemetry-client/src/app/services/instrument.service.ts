import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Instrument, InstrumentHealthResponse, CreateInstrumentRequest } from '../models/run.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class InstrumentService {
  private readonly baseUrl = `${environment.apiBaseUrl}/instruments`;

  constructor(private readonly http: HttpClient) {}

  list(limit = 50): Observable<Instrument[]> {
    const params = new HttpParams().set('limit', limit);
    return this.http.get<Instrument[]>(this.baseUrl, { params });
  }

  create(request: CreateInstrumentRequest): Observable<InstrumentHealthResponse> {
    return this.http.post<InstrumentHealthResponse>(this.baseUrl, request);
  }

  getHealth(id: string): Observable<InstrumentHealthResponse> {
    return this.http.get<InstrumentHealthResponse>(`${this.baseUrl}/${id}/health`);
  }
}
