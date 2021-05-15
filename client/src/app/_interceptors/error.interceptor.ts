import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private router: Router, private toastr: ToastrService) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((response) => {
        if (response) {
          switch (response.status) {
            case 400:
              if (response.error.errors) {
                const modalStateErrors = [];
                const responseErrors = response.error.errors;
                for (const key in responseErrors) {
                  if (responseErrors[key]) {
                    modalStateErrors.push(responseErrors[key]);
                  }
                }

                console.log(Array.prototype.concat.apply([], modalStateErrors));
                throw Array.prototype.concat.apply([], modalStateErrors); // could also use modalStateErrors.flat(1), using es2019
              } else {
                this.toastr.error(response.statusText, response.status);
              }
              break;
            case 401:
              this.toastr.error(response.statusText, response.status);
              break;
            case 404:
              this.router.navigateByUrl('/not-found');
              break;
            case 500:
              const navigationExtras: NavigationExtras = {
                state: { error: response.error },
              };
              this.router.navigateByUrl('/server-error', navigationExtras);
              break;
            default:
              this.toastr.error('Something unexpected went wrong!');
              console.log(response);
              break;
          }
        }
        return throwError(response);
      })
    );
  }
}
