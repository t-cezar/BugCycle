/**
 * Componentele board-ului. Sunt definite ca obiecte simple
 * Vue 3 vine din CDN, iar template-urile sunt string-uri
 */
window.BoardComponents = (function () {
    const IssueCard = {
        name: 'IssueCard',
        props: {
            issue: { type: Object, required: true },
            draggable: { type: Boolean, default: false },
            pending: { type: Boolean, default: false }
        },
        emits: ['dragstart', 'dragend'],
        computed: {
            typeIcon() {
                return { Bug: 'B', Task: 'T', Story: 'S' }[this.issue.type] || '?';
            }
        },
        template: `
            <article class="board-card"
                     :class="{ 'is-dragging': pending, 'is-locked': !draggable }"
                     :draggable="draggable"
                     @dragstart="$emit('dragstart', issue)"
                     @dragend="$emit('dragend')">
                <div class="board-card-head">
                    <span class="type-chip" :class="'type-' + issue.type.toLowerCase()"
                          :title="issue.type">{{ typeIcon }}</span>
                    <a :href="'/Issues/' + issue.id" class="issue-key">{{ issue.displayKey }}</a>
                    <span class="priority-dot" :class="'priority-' + issue.priority.toLowerCase()"
                          :title="'Prioritate: ' + issue.priority"></span>
                </div>

                <a :href="'/Issues/' + issue.id" class="board-card-title">{{ issue.title }}</a>

                <div v-if="issue.labels.length" class="label-strip">
                    <span v-for="label in issue.labels" :key="label.id"
                          class="label-chip" :style="{ backgroundColor: label.colorHex }">
                        {{ label.name }}
                    </span>
                </div>

                <footer class="board-card-foot">
                    <span v-if="issue.assignee" class="avatar avatar-sm"
                          :title="issue.assignee.fullName">{{ issue.assignee.initials }}</span>
                    <span v-else class="avatar avatar-sm avatar-empty" title="Neasignat">?</span>

                    <span v-if="issue.storyPoints !== null" class="story-points"
                          :title="issue.storyPoints + ' story points'">{{ issue.storyPoints }}</span>
                </footer>
            </article>
        `
    };

    const BoardColumn = {
        name: 'BoardColumn',
        components: { IssueCard },
        props: {
            title: { type: String, required: true },
            status: { type: String, required: true },
            statusKey: { type: String, required: true },
            issues: { type: Array, required: true },
            canEdit: { type: Boolean, default: false },
            draggedId: { type: [Number, null], default: null }
        },
        emits: ['drop-card', 'drag-start', 'drag-end'],
        data() {
            return { isOver: false, insertIndex: null };
        },
        computed: {
            points() {
                return this.issues.reduce((sum, i) => sum + (i.storyPoints || 0), 0);
            }
        },
        methods: {
            onDragOver(event) {
                if (!this.canEdit || this.draggedId === null) {
                    return;
                }

                event.preventDefault();
                this.isOver = true;
            },

            onDragLeave(event) {
                // dragleave se declanseaza si cand cursorul trece peste un card
                // din interior; verific ca a parasit coloana.
                if (!event.currentTarget.contains(event.relatedTarget)) {
                    this.isOver = false;
                    this.insertIndex = null;
                }
            },

            onCardDragOver(event, index) {
                if (!this.canEdit || this.draggedId === null) {
                    return;
                }

                event.preventDefault();

                // Jumatatea de sus a cardului insereaza inaintea lui, cea de jos dupa el
                const box = event.currentTarget.getBoundingClientRect();
                const after = event.clientY > box.top + box.height / 2;

                this.insertIndex = after ? index + 1 : index;
                this.isOver = true;
            },

            onDrop() {
                if (!this.canEdit || this.draggedId === null) {
                    return;
                }

                const position = this.insertIndex === null ? this.issues.length : this.insertIndex;

                this.isOver = false;
                this.insertIndex = null;

                this.$emit('drop-card', { status: this.status, position });
            }
        },
        template: `
            <section class="board-column"
                     :class="{ 'is-over': isOver, ['column-' + statusKey]: true }"
                     @dragover="onDragOver"
                     @dragleave="onDragLeave"
                     @drop.prevent="onDrop">
                <header class="board-column-head">
                    <h2>{{ title }}</h2>
                    <span class="board-column-count">
                        {{ issues.length }}<span v-if="points"> &middot; {{ points }} SP</span>
                    </span>
                </header>

                <div class="board-column-body">
                    <template v-for="(issue, index) in issues" :key="issue.id">
                        <div class="drop-marker" v-if="insertIndex === index"></div>
                        <div @dragover="onCardDragOver($event, index)">
                            <issue-card :issue="issue"
                                        :draggable="canEdit"
                                        :pending="draggedId === issue.id"
                                        @dragstart="$emit('drag-start', issue)"
                                        @dragend="$emit('drag-end')" />
                        </div>
                    </template>

                    <div class="drop-marker" v-if="insertIndex === issues.length && issues.length"></div>

                    <p v-if="!issues.length" class="board-column-empty">
                        {{ canEdit ? 'Trage un card aici' : 'Niciun issue' }}
                    </p>
                </div>
            </section>
        `
    };

    return { IssueCard, BoardColumn };
})();
