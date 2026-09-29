import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  isDarkMode = signal<boolean>(false);

  constructor() {
    const savedTheme = localStorage.getItem('becanet_theme');
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const shouldBeDark = savedTheme ? savedTheme === 'dark' : prefersDark;
    
    this.setDarkMode(shouldBeDark);
  }

  toggleTheme(): void {
    this.setDarkMode(!this.isDarkMode());
  }

  private setDarkMode(isDark: boolean): void {
    this.isDarkMode.set(isDark);
    localStorage.setItem('becanet_theme', isDark ? 'dark' : 'light');
    if (isDark) {
      document.body.classList.add('dark-theme');
    } else {
      document.body.classList.remove('dark-theme');
    }
  }
}