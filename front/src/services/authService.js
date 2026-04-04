import { api, setAuthToken } from '../api/api.js';


export const authorizeUser = async (email, password, navigate) => {
    try {
        const response = await api.post('auth/login', { email, password });
        const data = response.data;


        setAuthToken(data.token);

        const userToSave = {
            id: data.userId,
            name: data.displayName,
            role: data.role,
            token: data.token
        };
        localStorage.setItem('user', JSON.stringify(userToSave));

        switch (data.role) {
            case 'Customer':
                console.log('Роут для Customer');
                navigate('/customerpage');
                break;

            case 'Courier':
                console.log('Роут для Courier');
                break;

            case 'Restaurant':
                console.log('Роут для Restaurant');
                break;

            case 'Moderator':
                console.log('Роут для Moderator');
                break;

            default:
                console.warn('Неизвестная роль:', data.role);
                navigate('/login');
        }

    } catch (error) {
        console.error('Ошибка авторизации:', error);

        if (error.response && error.response.status === 401) {
            return { success: false, error: 'Неверный логин или пароль' };
        }

        return { success: false, error: 'Ошибка сети или сервера' };
    }
};