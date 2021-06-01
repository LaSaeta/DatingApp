import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { LikesParams } from '../_models/likesParams';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map();
  user: User;
  userParams: UserParams;

  constructor(
    private http: HttpClient,
    private accountService: AccountService
  ) {
    this.accountService.currentUser.pipe(take(1)).subscribe((user) => {
      this.user = user;
      this.userParams = new UserParams(user);
    });
  }

  getUserParams(): UserParams {
    return this.userParams;
  }

  setUserParams(params: UserParams): void {
    this.userParams = params;
  }

  resetUserParams(): UserParams {
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }

  getMembers(userParams: UserParams): any {
    const cachedResponse = this.memberCache.get(
      Object.values(userParams).join('-')
    );

    if (cachedResponse) {
      return of(cachedResponse);
    }

    let params = this.getPaginationHeaders(
      userParams.pageNumber,
      userParams.pageSize
    );

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    return this.getPaginatedResult<Member[]>(
      `${this.baseUrl}users`,
      params
    ).pipe(
      map((response) => {
        this.memberCache.set(Object.values(userParams).join('-'), response);
        return response;
      })
    );
  }

  getMember(userName: string): Observable<Member> {
    const member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((user: Member) => user.userName === userName);

    if (member) {
      return of(member);
    }

    return this.http.get<Member>(`${this.baseUrl}users\\${userName}`);
  }

  updateMember(member: Member): Observable<any> {
    return this.http.put(`${this.baseUrl}users`, member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    );
  }

  addLike(member: Member): Observable<any> {
    return this.http.post(`${this.baseUrl}likes/${member.userName}`, {});
  }

  getLikes(likesParams: LikesParams): Observable<any> {
    let params = this.getPaginationHeaders(
      likesParams.pageNumber,
      likesParams.pageSize
    );

    params = params.append('predicate', likesParams.predicate);

    return this.getPaginatedResult<Partial<Member[]>>(
      `${this.baseUrl}likes`,
      params
    );
  }

  setMainPhoto(photoId: number): Observable<any> {
    return this.http.put(`${this.baseUrl}users/set-main-photo/${photoId}`, {});
  }

  deletePhoto(photoId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}users/delete-photo/${photoId}`);
  }

  private getPaginatedResult<T>(
    url: string,
    params: HttpParams
  ): Observable<PaginatedResult<T>> {
    return this.http.get<T>(url, { observe: 'response', params }).pipe(
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

  private getPaginationHeaders(
    pageNumber: number,
    pageSize: number
  ): HttpParams {
    let params = new HttpParams();

    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());

    return params;
  }
}
