export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  username: string;
  role: string;
  expiresAt: string;
}

export interface RefreshTokenRequest {
  token: string;
  refreshToken: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

export interface AuthUser {
  id: number;
  username: string;
  email: string;
  role: string;
  organisationId?: string;
  // Champs utilisés par le header
  name?: string;
  prenom?: string;
  nom?: string;
  fonction: string; 
}