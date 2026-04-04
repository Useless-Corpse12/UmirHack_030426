import {Link, useNavigate} from "react-router-dom";
import Logo from "../assets/logo.svg";


function Register() {
const navigate = useNavigate();
    return(
        <div className="Reg-page">
            <div>
                <img src={Logo} alt="logo"/>
                <h1>Регистрация</h1>
                <h6>Введите email</h6>
                <div><input email /></div>
                <h6>Введите пароль</h6>
                <div><input password /></div>
                <h6>Введите пароль снова</h6>
                <div><input password again /></div>
                <button> Регистрация </button>
                <h6>Аккант всё же есть?</h6>
                <button onClick={() => navigate('/login')}> Назад </button>
                <h6>Курьер или организация-партнёр?
                    вы можете оставить заявку на регистрацию в нашем сервисе!</h6>
                <Link to={'/zayavka'}>Оставить заявку</Link>
            </div>
        </div>
    )
}

export default Register;