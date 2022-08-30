import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dating app';
  users: any;

  constructor(private accountService:AccountService){}

  ngOnInit() {
    this.saveCurrentUser();
  }

  saveCurrentUser(){
    const user: User = JSON.parse(localStorage.getItem('user'));
    this.accountService.saveCurrentUser(user);
  }
}
