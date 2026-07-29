import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Instrument, InstrumentHealth, CreateInstrumentRequest } from '../models/run.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class InstrumentService {
  private readonly baseUrl = `${environment.apiBaseUrl}/instruments`;

  constructor(private readonly http: HttpClient) {}

  create(request: CreateInstrumentRequest): Observable<Instrument> {
    return this.http.post<Instrument>(this.baseUrl, request);
  }

  getHealth(id: string): Observable<InstrumentHealth> {
    return this.http.get<InstrumentHealth>(`${this.baseUrl}/${id}/health`);
  }
}
