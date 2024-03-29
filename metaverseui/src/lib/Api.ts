import axios from 'axios';
import moment from 'moment';
import Config from '@/lib/Config.ts';

export class AuthenticationData {
    get expiryTime(): moment.Moment | null {
        const time = localStorage.getItem('x-md-expirydate');
        if (!time) return null;
        return moment(time);
    }
    set expiryTime(val: moment.Moment | null) {
        if (!val) localStorage.removeItem('x-md-expirydate');
        else localStorage.setItem('x-md-expirydate', val.toString());
    }
    get accessToken(): string | null {
        return localStorage.getItem('x-md-accesstoken');
    }
    set accessToken(val: string | null) {
        if (val) localStorage.setItem('x-md-accesstoken', val);
        else localStorage.removeItem('x-md-accesstoken');
    }
    get web3Auth(): string | null {
        return localStorage.getItem('x-md-web3auth');
    }
    set web3Auth(val: string | null) {
        if (val) localStorage.setItem('x-md-web3auth', val);
        else localStorage.removeItem('x-md-web3auth');
    }
    get currentAddress() : string | null {
        return localStorage.getItem('x-md-ethaddr');
    }
    set currentAddress(val: string | null) {
        if (val) localStorage.setItem('x-md-ethaddr', val);
        else localStorage.removeItem('x-md-ethaddr');
    }
}

export class Api {
    public authData: AuthenticationData = new AuthenticationData();

    constructor() {
        const accessToken = this.authData.accessToken
        if (!accessToken) return;
        const expiryTime = this.authData.expiryTime;
        if (!expiryTime) return;
        if (expiryTime < moment()) return;
        axios.defaults.headers.common.Authorization = `Bearer ${accessToken}`;
        const web3Auth = this.authData.web3Auth;
        if (!web3Auth) return;
        axios.defaults.headers.common['X-Web3-Auth'] = `${web3Auth}:${expiryTime.toDate().getTime()}`;
    }

    public create() {
        return axios.create({
            baseURL: Config.apiUrl
        });
    }

    public applyDiscordCredentials(params: URLSearchParams) {
        const accessToken = params.get('access_token');
        const expiresIn = params.get('expires_in');
        this.authData.accessToken = accessToken;
        this.authData.expiryTime = moment().add(Number(expiresIn), 'seconds');
        axios.defaults.headers.common.Authorization = `Bearer ${accessToken}`;
    }

    public isDiscordAuthenticated() {
        return !!axios.defaults.headers.common.Authorization;
    }

    public applyEthereumCredentials(signature: string) {
        this.authData.web3Auth = signature;
        axios.defaults.headers.common['X-Web3-Auth'] = `${signature}:${this.authData.expiryTime?.toDate().getTime()}`;
    }

    public isEthereumAuthenticated() {
        return !!axios.defaults.headers.common['X-Web3-Auth'];
    }

    public initiateDiscordLogin() {
        const params = new URLSearchParams({
            'client_id': "790559120603873320",
            'redirect_uri': `${window.location.protocol}//${window.location.hostname == 'localhost' ? window.location.host : window.location.hostname}/login/callback`,
            'response_type': "token",
            scope: "identify guilds"
        });
        window.location.href = `https://discord.com/api/oauth2/authorize?${params.toString()}`;
    }
}

export default new Api();