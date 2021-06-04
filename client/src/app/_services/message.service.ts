import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import {
  getPaginatedResult,
  getPaginationHeaders,
} from '../_helpers/paginationHelper';
import { Message } from '../_models/message';
import { PaginatedResult } from '../_models/pagination';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getMessages(
    pageNumber: number,
    pageSize: number,
    container: string
  ): Observable<PaginatedResult<Message[]>> {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);

    return getPaginatedResult<Message[]>(
      `${this.baseUrl}messages`,
      params,
      this.http
    );
  }

  getMessageThread(userName: string): Observable<Message[]> {
    return this.http.get<Message[]>(
      `${this.baseUrl}messages/thread/${userName}`
    );
  }

  sendMessage(userName: string, content: string): Observable<Message> {
    return this.http.post<Message>(`${this.baseUrl}messages`, {
      recipientUserName: userName,
      content,
    });
  }

  deleteMessage(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}messages/${id}`);
  }
}
