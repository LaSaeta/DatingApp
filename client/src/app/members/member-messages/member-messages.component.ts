import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm: NgForm;
  @Input() userName: string;
  messageContent: string;

  constructor(public messageService: MessageService) {}

  ngOnInit(): void {}

  sendMessage(): void {
    this.messageService
      .sendMessage(this.userName, this.messageContent)
      .then(() => {
        this.messageForm.reset();
      });
  }
}
