export default {
    get apiUrl():string {
        return (window as any).$metaverse.apiUrl;
    }
}