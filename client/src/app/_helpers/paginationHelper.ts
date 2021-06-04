import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { PaginatedResult } from '../_models/pagination';

export function getPaginatedResult<T>(
  url: string,
  params: HttpParams,
  http: HttpClient
): Observable<PaginatedResult<T>> {
  return http.get<T>(url, { observe: 'response', params }).pipe(
    map((response) => {
      const paginatedResult = new PaginatedResult<T>();
      paginatedResult.result = response.body as T;
      if (response.headers.get('Pagination')) {
        paginatedResult.pagination = JSON.parse(
          response.headers.get('Pagination') as string
        );
      }
      return paginatedResult;
    })
  );
}

export function getPaginationHeaders(
  pageNumber: number,
  pageSize: number
): HttpParams {
  let params = new HttpParams();

  params = params.append('pageNumber', pageNumber.toString());
  params = params.append('pageSize', pageSize.toString());

  return params;
}
