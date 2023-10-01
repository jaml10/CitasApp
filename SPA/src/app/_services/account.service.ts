import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { IUser } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = "https://localhost:5001/api/";
  private currentUserSoure = new BehaviorSubject<IUser | null>(null);
  currentUser$ = this.currentUserSoure.asObservable();

  constructor(private http: HttpClient) { }

  login(model: IUser){
    return this.http.post<IUser>(this.baseUrl + "account/login", model).pipe(
      map((response: IUser) => {
        const user = response;
        if (user) {
          localStorage.setItem("user", JSON.stringify(user));
          this.currentUserSoure.next(user);
        }
      })
    );
  }

  setCurrentUser(user: IUser){
    this.currentUserSoure.next(user);
  }

  logout() {
    localStorage.removeItem("user");
  }
}
