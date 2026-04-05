import axios from 'axios'

export const api = axios.create({
 baseURL: 'https://api.labofdev.ru/api/',
 headers: {'Content-Type': 'application/json',
         "Access-Control-Allow-Origin" : "*",}
})

export const login = async (email, password) => {
 const response = await api.post('auth/login', { email, password });
 return response.data;
}

export const setAuthToken = (token) => {
 if (token) {
  api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
 } else {
  delete api.defaults.headers.common['Authorization'];
 }
};