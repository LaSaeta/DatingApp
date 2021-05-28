import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  model: any = {};
  registerForm: FormGroup;
  genderControl: FormControl;
  userNameControl: FormControl;
  knownAsControl: FormControl;
  dateOfBirthControl: FormControl;
  cityControl: FormControl;
  countryControl: FormControl;
  passwordControl: FormControl;
  confirmPasswordControl: FormControl;
  maxDate: Date;
  validationErrors: string[] = [];

  constructor(
    private accountService: AccountService,
    private formBuilder: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
    this.setControls();
  }

  setControls(): void {
    this.genderControl = this.registerForm.controls.gender as FormControl;
    this.userNameControl = this.registerForm.controls.userName as FormControl;
    this.knownAsControl = this.registerForm.controls.knownAs as FormControl;
    this.dateOfBirthControl = this.registerForm.controls
      .dateOfBirth as FormControl;
    this.cityControl = this.registerForm.controls.city as FormControl;
    this.countryControl = this.registerForm.controls.country as FormControl;
    this.passwordControl = this.registerForm.controls.password as FormControl;
    this.confirmPasswordControl = this.registerForm.controls
      .confirmPassword as FormControl;
  }

  initializeForm(): void {
    this.registerForm = this.formBuilder.group({
      gender: [''],
      userName: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: [
        '',
        [Validators.required, Validators.minLength(4), Validators.maxLength(8)],
      ],
      confirmPassword: [
        '',
        [Validators.required, this.matchValues('password')],
      ],
    });
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      const controls = control?.parent?.controls as {
        [key: string]: AbstractControl;
      };
      let matchToControl = null;
      if (controls) {
        matchToControl = controls[matchTo];
      }
      return control?.value === matchToControl?.value
        ? null
        : { noMatch: true };
    };
  }

  register(): void {
    this.accountService.register(this.registerForm.value).subscribe(
      (response) => {
        this.router.navigateByUrl('/members');
      },
      (error) => {
        this.validationErrors = error;
      }
    );
  }

  cancel(): void {
    this.cancelRegister.emit(false);
  }
}
