import http from './index'

export interface LoginRequest {
  username: string
  password: string
}

export interface UserInfo {
  id: number
  username: string
  displayName: string
}

export interface LoginResponse {
  token: string
  expiresAt: string
  userInfo: UserInfo
}

export const authApi = {
  login(data: LoginRequest) {
    return http.post<LoginResponse>('/auth/login', data)
  }
}
