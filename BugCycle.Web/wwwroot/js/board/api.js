/**
 * Accesul la API din board. Autentificarea merge pe acelasi cookie ca restul
 * aplicatiei (nu exista token de gestionat in JavaScript)
 *
 * Program.cs configureaza cookie-ul sa raspunda cu 401 si 403 pe /api in loc
 * de redirect catre login. Evit cazul in care fetch ar urma redirectul si ar
 * primi pagina de login cu status 200 (ar crede ca apelul a reusit)
 */
window.BoardApi = (function () {
    async function request(url, options) {
        const response = await fetch(url, {
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
            ...options
        });

        if (response.status === 204) {
            return null;
        }

        if (!response.ok) {
            throw await toError(response);
        }

        return response.json();
    }

    /**
     * Transforma raspunsul de eroare in ceva afisabil. Middleware-ul global
     * trimite ProblemDetails, mesajul de business ajunge intact la
     * utilizator (ex: un issue nu poate fi mutat in Done fara assignee).
     */
    async function toError(response) {
        let message;

        switch (response.status) {
            case 401:
                message = 'Sesiunea a expirat. Reincarca pagina si autentifica-te din nou.';
                break;
            case 403:
                message = 'Nu ai permisiunea sa muti carduri in acest proiect.';
                break;
            default:
                message = null;
        }

        if (!message) {
            try {
                const problem = await response.json();
                message = problem.detail || problem.title || 'Operatia a esuat.';
            } catch {
                message = 'Operatia a esuat.';
            }
        }

        const error = new Error(message);
        error.status = response.status;
        return error;
    }

    return {
        getSprintIssues(sprintId) {
            return request(`/api/sprints/${sprintId}/issues`, { method: 'GET' });
        },

        getBacklogIssues(projectId) {
            return request(`/api/issues?projectId=${projectId}&backlogOnly=true`, { method: 'GET' });
        },

        moveIssue(issueId, status, position) {
            return request(`/api/issues/${issueId}/move`, {
                method: 'PATCH',
                body: JSON.stringify({ status, position })
            });
        },

        setSprint(issueId, sprintId) {
            return request(`/api/issues/${issueId}/sprint`, {
                method: 'PATCH',
                body: JSON.stringify({ sprintId })
            });
        }
    };
})();
