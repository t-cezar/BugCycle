/**
 * Board-ul kanban. Este singura parte a aplicatiei construita ca SPA. Am stare in client
 * fiindca drag and drop-ul trebuie sa raspunda instant, fara reincarcare.
 */
(function () {
    const { createApp } = Vue;
    const { BoardColumn } = window.BoardComponents;

    const COLUMNS = [
        { status: 'ToDo', key: 'todo', title: 'To Do' },
        { status: 'InProgress', key: 'progress', title: 'In Progress' },
        { status: 'InReview', key: 'review', title: 'In Review' },
        { status: 'Done', key: 'done', title: 'Done' }
    ];

    const root = document.getElementById('board-app');

    if (!root) {
        return;
    }

    createApp({
        components: { BoardColumn },

        data() {
            return {
                projectId: Number(root.dataset.projectId),
                sprintId: root.dataset.sprintId ? Number(root.dataset.sprintId) : null,
                canEdit: root.dataset.canEdit === 'true',
                columns: COLUMNS,
                issues: [],
                loading: true,
                error: null,
                notice: null,
                draggedId: null
            };
        },

        computed: {
            totalPoints() {
                return this.issues.reduce((sum, i) => sum + (i.storyPoints || 0), 0);
            },

            donePoints() {
                return this.issues
                    .filter(i => i.status === 'Done')
                    .reduce((sum, i) => sum + (i.storyPoints || 0), 0);
            },

            progressPercent() {
                if (!this.issues.length) {
                    return 0;
                }

                const done = this.issues.filter(i => i.status === 'Done').length;
                return Math.round((100 * done) / this.issues.length);
            }
        },

        async mounted() {
            await this.load();
        },

        methods: {
            async load() {
                if (this.sprintId === null) {
                    this.loading = false;
                    return;
                }

                this.loading = true;
                this.error = null;

                try {
                    this.issues = await window.BoardApi.getSprintIssues(this.sprintId);
                } catch (err) {
                    this.error = err.message;
                } finally {
                    this.loading = false;
                }
            },

            issuesFor(status) {
                return this.issues
                    .filter(i => i.status === status)
                    .sort((a, b) => a.boardOrder - b.boardOrder || a.id - b.id);
            },

            onDragStart(issue) {
                this.draggedId = issue.id;
                this.notice = null;
                this.error = null;
            },

            onDragEnd() {
                this.draggedId = null;
            },

            /**
             * Actualizare optimista: mut cardul in stare imediat, gestul sa
             * para instant, dar pastrez o copie a listei. Daca serverul refuza
             * (ex: Done fara assignee, sau rol de Viewer), reven exact
             * la starea de dinainte si arat motivul.
             * 
             * Asa fac ca interfata sa nu afiseze o mutare care nu s-a salvat niciodata.
             */
            async onDrop(targetStatus, position) {
                const issueId = this.draggedId;
                this.draggedId = null;

                if (issueId === null || !this.canEdit) {
                    return;
                }

                const moved = this.issues.find(i => i.id === issueId);

                if (!moved) {
                    return;
                }

                const snapshot = this.issues.map(i => ({ ...i }));
                const samePlace =
                    moved.status === targetStatus &&
                    this.issuesFor(targetStatus).findIndex(i => i.id === issueId) === position;

                if (samePlace) {
                    return;
                }

                this.applyLocalMove(moved, targetStatus, position);

                try {
                    const updated = await window.BoardApi.moveIssue(issueId, targetStatus, position);

                    // Sincronizez cu ce a decis serverul
                    // pentru BoardOrder si pentru ClosedAt.
                    const index = this.issues.findIndex(i => i.id === updated.id);

                    if (index !== -1) {
                        this.issues.splice(index, 1, updated);
                    }

                    await this.refreshOrders(targetStatus);
                } catch (err) {
                    this.issues = snapshot;
                    this.error = err.message;
                }
            },

            applyLocalMove(moved, targetStatus, position) {
                const target = this.issuesFor(targetStatus).filter(i => i.id !== moved.id);

                moved.status = targetStatus;
                target.splice(Math.min(position, target.length), 0, moved);

                target.forEach((issue, index) => {
                    issue.boardOrder = index;
                });
            },

            //Serverul renumeroteaza toata coloana la fiecare mutare, deci recitesc ordinea

            async refreshOrders(status) {
                try {
                    const fresh = await window.BoardApi.getSprintIssues(this.sprintId);

                    fresh
                        .filter(f => f.status === status)
                        .forEach(f => {
                            const local = this.issues.find(i => i.id === f.id);

                            if (local) {
                                local.boardOrder = f.boardOrder;
                            }
                        });
                } catch {
                    // Ordinea exacta nu merita un mesaj de eroare: mutarea a
                    // fost deja salvata, iar un refresh o va corecta.
                }
            },

            dismissError() {
                this.error = null;
            }
        },

        template: `
            <div class="board-shell">
                <div v-if="error" class="alert alert-danger alert-dismissible fade show">
                    {{ error }}
                    <button type="button" class="btn-close" @click="dismissError"></button>
                </div>

                <div v-if="sprintId === null" class="empty-state">
                    <h2>Niciun sprint activ</h2>
                    <p>
                        Board-ul afiseaza sprintul activ al proiectului. Porneste un
                        sprint din pagina de sprinturi ca sa apara cardurile aici.
                    </p>
                    <a class="btn btn-primary" :href="'/Projects/' + projectId + '/Sprints'">
                        Mergi la sprinturi
                    </a>
                </div>

                <template v-else>
                    <div v-if="loading" class="board-loading">Se incarca board-ul...</div>

                    <template v-else>
                        <div class="board-summary">
                            <div class="progress board-progress">
                                <div class="progress-bar" :style="{ width: progressPercent + '%' }"></div>
                            </div>
                            <span class="board-summary-text">
                                {{ progressPercent }}% finalizat &middot;
                                {{ donePoints }} / {{ totalPoints }} story points
                            </span>
                            <span v-if="!canEdit" class="board-readonly">
                                Ai acces doar de citire, cardurile nu pot fi mutate.
                            </span>
                        </div>

                        <div class="board-columns">
                            <board-column v-for="column in columns"
                                          :key="column.status"
                                          :title="column.title"
                                          :status="column.status"
                                          :status-key="column.key"
                                          :issues="issuesFor(column.status)"
                                          :can-edit="canEdit"
                                          :dragged-id="draggedId"
                                          @drag-start="onDragStart"
                                          @drag-end="onDragEnd"
                                          @drop-card="onDrop($event.status, $event.position)" />
                        </div>
                    </template>
                </template>
            </div>
        `
    }).mount('#board-app');
})();
