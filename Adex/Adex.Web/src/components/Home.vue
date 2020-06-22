<template>
    <div class="home">
        <h1>{{ msg }}</h1>
        <input type="text"  />
        <div id="container" class="svg-container">
        </div>
    </div>
</template>

<script lang="ts">
    import { Component, Prop, Vue } from 'vue-property-decorator';

    @Component
    export default class Home extends Vue {
        @Prop() private msg!: string;
    }


</script>

<style scoped>
    .svg-container {
        display: inline-block;
        position: relative;
        width: 100%;
        padding-bottom: 100%;
        vertical-align: top;
        overflow: hidden;
    }

    .svg-content {
        display: inline-block;
        position: absolute;
        top: 0;
        left: 0;
        /*background-color: dimgray;*/
    }

    .node {
        font: 300 11px "Helvetica Neue", Helvetica, Arial, sans-serif;
        fill: #bbb;
    }

        .node:hover {
            fill: #000;
        }

    .link {
        stroke: steelblue;
        stroke-opacity: 0.4;
        fill: none;
        pointer-events: none;
    }

    .node:hover,
    .node--source,
    .node--target {
        font-weight: 700;
    }

    .node--source {
        fill: #2ca02c;
    }

    .node--target {
        fill: #d62728;
    }

    .link--source,
    .link--target {
        stroke-opacity: 1;
        stroke-width: 2px;
    }

    .link--source {
        stroke: #d62728;
    }

    .link--target {
        stroke: #2ca02c;
    }
</style>
