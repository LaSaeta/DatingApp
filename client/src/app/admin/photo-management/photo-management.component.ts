import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css'],
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[] = [];

  constructor(
    private adminService: AdminService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getPhotosForApproval();
  }

  getPhotosForApproval(): void {
    this.adminService.getPhotosForApproval().subscribe((photos) => {
      this.photos = photos;
    });
  }

  approvePhoto(photo: Photo): void {
    this.adminService.approvePhoto(photo.id).subscribe(
      () => {
        this.toastr.success('Approved!');
        this.photos = this.photos.filter((p) => p.id !== photo.id);
      },
      (error) => {
        this.toastr.error(error);
      }
    );
  }

  rejectPhoto(photo: Photo): void {
    this.adminService.rejectPhoto(photo.id).subscribe(
      () => {
        this.toastr.warning('Rejected!');
        this.photos = this.photos.filter((p) => p.id !== photo.id);
      },
      (error) => {
        this.toastr.error(error);
      }
    );
  }
}
