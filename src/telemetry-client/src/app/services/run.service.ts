import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Run, RunTimeline, CreateRunRequest } from '../models/run.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class RunService {
  private readonly baseUrl = `${environment.apiBaseUrl}/runs`;

  constructor(private readonly http: HttpClient) {}

  list(limit = 50): Observable<Run[]> {
    const params = new HttpParams().set('limit', limit);
    return this.http.get<Run[]>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Run> {
    return this.http.get<Run>(`${this.baseUrl}/${id}`);
  }

  getTimeline(id: string): Observable<RunTimeline> {
    return this.http.get<RunTimeline>(`${this.baseUrl}/${id}/timeline`);
  }

  create(request: CreateRunRequest): Observable<Run> {
    return this.http.post<Run>(this.baseUrl, request);
  }

  queue(id: string, actor?: string): Observable<Run> {
    return this.transition(id, 'queue', actor);
  }

  start(id: string, actor?: string): Observable<Run> {
    return this.transition(id, 'start', actor);
  }

  complete(id: string, actor?: string): Observable<Run> {
    return this.transition(id, 'complete', actor);
  }

  fail(id: string, actor?: string): Observable<Run> {
    return this.transition(id, 'fail', actor);
  }

  cancel(id: string, actor?: string): Observable<Run> {
    return this.transition(id, 'cancel', actor);
  }

  downloadSupportBundle(id: string): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/${id}/support-bundle`, null, {
      responseType: 'blob',
    });
  }

  private transition(id: string, action: string, actor?: string): Observable<Run> {
    let params = new HttpParams();
    if (actor) {
      params = params.set('actor', actor);
    }
    return this.http.post<Run>(`${this.baseUrl}/${id}/${action}`, null, { params });
  }
}
