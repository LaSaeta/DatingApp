import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'Dating App';
  users: any;

  constructor(private accountService: AccountService) {}

  ngOnInit(): void {
    this.setCurrentUser();
  }

  setCurrentUser(): void {
    const userJson = localStorage.getItem('user');
    let user: User = {
      userName: '',
      token: '',
      photoUrl: '',
      gender: '',
      knownAs: '',
      roles: ['Member'],
    };

    if (userJson) {
      user = JSON.parse(userJson);
      this.accountService.setCurrentUser(user);
    }
  }
}
