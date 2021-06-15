import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getUsersWithRoles(): Observable<any> {
    return this.http.get<Partial<User[]>>(
      `${this.baseUrl}admin/users-with-roles`
    );
  }

  updateUserRoles(userName: string, roles: string[]): Observable<any> {
    return this.http.post(
      `${this.baseUrl}admin/edit-roles/${userName}?roles=${roles}`,
      {}
    );
  }

  getPhotosForApproval(): Observable<any> {
    return this.http.get(`${this.baseUrl}admin/photos-to-moderate`);
  }

  approvePhoto(photoId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}admin/approve-photo/${photoId}`, {});
  }

  rejectPhoto(photoId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}admin/reject-photo/${photoId}`, {});
  }
}
